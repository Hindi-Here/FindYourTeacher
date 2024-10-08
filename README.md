# FindYourTeacher
Бот, разработанный для студентов академии ИМСИТ в г. Краснодар. Бот работает только на основе базы данных преподавательского состава академии и не имеет привязок со студентами.

**Бот поддерживает следующие команды:**
+ **/start** - возвращение к началу работы (ранее сохраненные команды стираются)
+ **/now** - выдает текущее занятие у преподавателя
+ **/list** - показывает расписание преподавателя на целый день
+ **/link** - вывод ссылки на сайт с графиком занятий преподавателя

## Особенности
**FindYourTeacher запоминает каждую команду, которую ввел пользователь.** Таким образом нет необходимости при каждом запросе прописывать конкретную команду, что позволяет изначально настроить удобный формат вывода данных под себя. Команды хранятся в ассоциативном контейнере `private static readonly Dictionary<long, string> userCommands`. Это не дает произойти нарушениям работы бота из-за перезаписи команд сторонними пользователями.

При использовании бота учитывайте, что **бот чувствителен к регистру**. Это означает, что при записи "Ткаценко и.С." выдаст ошибку, поскольку инициал "и" не заглавный.

**FindYourTeacher и взаимодействие с пользователем.** В воскресные дни, а также при запросе в позднее время (когда заканчивается последняя лекция) бот будет сообщать пользователю, что сейчас ни у кого нет занятий и все заслуженно отдыхают.

## Парсинг таблиц

**FindToTeacher использует библиотеку AngleSharp** для получения данных из открытых таблиц академии ИМСИТ. Имеется два вида таблиц, которые обрабатываются ботом: [общий список](https://imsit.ru/timetable/teach/Praspisan.html) преподавателей ВУЗа, [индивидуальный график](https://imsit.ru/timetable/teach/m1.html) занятий отдельного преподавателя.

```C#
var document = GetHtmlCode("https://imsit.ru/timetable/teach/Praspisan.html");
return document.QuerySelectorAll("tr")
               .Select(row => row.QuerySelector("td").TextContent.Trim())
               .Select(text => text.Split(',')[0])
               .ToArray();
```

Общая таблица обрабатывается и записывается в контейнер `private readonly static string[] Teachers`. В первую очередь, такое решение обосновано снижением нагрузки на обработку данных, возникающую при парсинге нескольких страниц при каждой запросе. `Teachers` заполняется лишь единожды - при запуске программы.

Для получения данных из индивидуальной таблицы применяются данные из класса `TimeVariable`, хранящем сведения о времени запроса: индекс дня недели, индекс времени занятия, четность или нечетность недели. Все эти сведения в дальнейшем используются для ориентации в индивидуальном графике.

```C#
TimeVariable timeVariable = TimeRequestInfo();

string tableSelector = timeVariable.WeekParity == "четная" ? "body > table:nth-child(3)" : "body > table:nth-child(6)";
var table = document.QuerySelector(tableSelector);

var ResultRequest = table.QuerySelectorAll("tr")[timeVariable.DayIndex]
                         .QuerySelectorAll("td")[timeVariable.TimeIndex];

return CellOutOfRangeException(timeVariable) == false ? LessonTime[timeVariable.TimeIndex] + ResultRequest.TextContent
                                                      : "Сейчас ни у кого нет занятий. Все отдыхают!";
```

`DayIndex` устанавливает позицию по строкам (tr), а `TimeIndex` отвечает за столбцы (td). Поскольку две таблицы (для четной и нечетной недели) полностью идентичны по html коду - не предоставляется возможности удобного обращения к таблицам. Поэтому используется обращение по уникальному селектору для каждой таблицы (`body > table:nth-child(3)` и `body > table:nth-child(6)`).

Метод `CellOutOfRangeException(Timevariable timeVariable)` обеспечивает обработку ошибок в случаях, когда достигаются ячейки с null значением в таблице.

## Работа бота на скриншотах:
<img src="https://github.com/user-attachments/assets/70e9138c-f0bd-4496-a9f6-38e8244ecac2" width="400" height="450">

<img src="https://github.com/user-attachments/assets/aaa15bbb-60ee-4c68-967d-ad2243d2101f" width="400" height="450">

<img src="https://github.com/user-attachments/assets/1d02a446-fd44-430c-a7cf-5f36f8979b02" width="400" height="450">
