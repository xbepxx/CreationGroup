using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyGroupPlugin
{
    #region ЗАНЯТИЕ 3. ПЛАГИН "КОПИРОВАНИЕ ГРУППЫ ОБЪЕКТОВ". ЧАСТЬ 2.
    //Обработка исключений
    [TransactionAttribute(TransactionMode.Manual)]

    public class CopyGroup : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        /*
         методы, вызывающие исключения, это PickObject и PickPoint
        */
        {
            try
            {
                UIApplication uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;
                //ISelectionfilter - интерфейс с 2-мя методами: AllowElement (определяет, является ли элемент, над которым курсор мыши разрешенным) и AllowReference (тоже, но с ссылками).
                GroupPickFilter groupPickFilter = new GroupPickFilter(); //передаём экземпляр в PickObject
                Reference reference = uidoc.Selection.PickObject(ObjectType.Element, groupPickFilter, "Выберите группу элементов");
                Element element = doc.GetElement(reference);
                Group group = element as Group;
                XYZ groupCenter = GetElementCenter(group); //определяем центр комнаты
                Room room = GetRoomByPoint(doc, groupCenter);
                XYZ roomCenter = GetElementCenter(room); //находим центр комнаты
                XYZ offSet = groupCenter - roomCenter; //определяем смещение группы относительно центра

                XYZ point = uidoc.Selection.PickPoint("Выберите точку вставки");

                Room roomins = GetRoomByPoint(doc, point); //определяем комнату, которой принадлежит выбранная точка
                XYZ roomCenterins = GetElementCenter(roomins); //определяем центр комнаты
                XYZ offSetins = roomCenterins+ offSet;

                Transaction transaction = new Transaction(doc);
                transaction.Start("Копирование группы элементов");
                doc.Create.PlaceGroup(offSetins, group.GroupType);
                transaction.Commit();
            }
            #region Exception ESC
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) //исключение esc
            {
                return Result.Cancelled; //результат - отмена
            }
            catch (Exception ex) //для всех прочих
            {
                message = ex.Message; //передаём в Execute сообщение об ошибге, генерируемое Revit
                return Result.Failed;
            }
            #endregion
            return Result.Succeeded;
            
        }
        #region Point
        public XYZ GetElementCenter(Element element) //метод, получающий элемент и возвращающий точку
        {
            BoundingBoxXYZ boundin=element.get_BoundingBox(null); //"рамка" вокруг группы. В BoundingBoxXYZ min - левый нижний дальний угол, а max - правый верхний ближний
            return (boundin.Min + boundin.Max) / 2;
        }
        #endregion
        #region Поиск комнаты
        public Room GetRoomByPoint(Document doc, XYZ point) //ищем комнату, на которой точка. Для фильтра нужен документ
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms); //фильтр по комнатам
            foreach(Element e in collector)
            {
                Room room = e as Room; //рекомендуемое преобразование
                if (room!=null) //если преобразование успешно
                {
                    if (room.IsPointInRoom(point)) //если точка с заданными координатами попадает в комнату, то
                        return room;
                }
            }
            return null; //если не находим комнату, которой принадлежит точка, то null
        }
        #endregion
    }
    #region Фильтр по группе
    public class GroupPickFilter : ISelectionFilter
    {
        //Проверка. Т.к. всё наследуется от Element, то у объектов может быть ряд общих свойств: имя, уровень и т.д. Выберем Category, и определим Id со свойством InterValue, а значения находятся в BuildInCategory.
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
                return true;
            else
                return false;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    #endregion
    #endregion
}
