using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lesson_1
{
    class Lesson1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        /*
         commandData позволяет получить доступ к базе данных Revit, базе данных, открытомуц документу
        message - отсюда возкращается комментарий в виде результата выполнения метода
        elements - элементы, которые будут подсвечены, если будет неудача
        */
        {
            UIApplication uiapp = commandData.Application; //словно заходим в Revit, обращаемся к uiapp
            UIDocument uidoc = uiapp.ActiveUIDocument; //забираем интерфесй текущего документа
            //как варик UIDocument uidoc=commandData.Application.ActiveUIDocument
            Document doc = uidoc.Document; //обращаемся к базе документа
            //Selection - всё, что касается выбора. Reference - ссылка на объект (внутренний идентификатор)
            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Выберите группу элементов"); //получаем ссылку на группу объектов
            /* Мы получаем ссылку!!! Не, блядь, сам объект!!! И об этом, конечно же, нихуя не стоило, говорить в предыдущем модуле!!! И получаем мы, ебать его в рот, объект, через GetElement!!!*/
            Element element = doc.GetElement(reference); //в качестве аргумента передаем ссылку, или id. Element - родительский, или базовый класс для всех элементов Revit Api (как Object в C#)
            //для того, чтобы иметь возможность взаимодействовать с элементом, как группой, надо снова преобразовать в группу, о чём, опять же, не стоило говорить в предыдущем модуле!
            Group group = element as Group; //Можно и через (Group)element, но тогда можно обсосаться и получить исключение
            /*Попросим юзера выбрать точку, куда вставить группу. XYZ - класс точек*/
            XYZ point = uidoc.Selection.PickPoint("Выберите точку вставки");
            /*А теперь вставим точку группу в точку*/
            Transaction transaction = new Transaction(doc); //в качестве аргумента нужна ссылка на документ. Вообще всё, что касается создания чего-либо - Autodesk.revit.creation
            /* Вообще общий смысл таков: обращаемся к Document, потом к свойству, потом к методу*/
            transaction.Start("Копирование группы элементов");
            doc.Create.PlaceGroup(point, group.GroupType);
            transaction.Commit();

            return Result.Succeeded;
        }
    }
}
