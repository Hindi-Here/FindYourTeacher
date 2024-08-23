# FindToTeacher
Бот, разработанный для студентов академии ИМСИТ в г. Краснодар. Бот работает только на основе базы данных преподавательского состава академии и не имеет привязок со студентами.

**Бот поддерживает следующие команды:**
+ **/start** - возвращение к началу работы (ранее сохраненные команды стираются)
+ **/now** - выдает текущее занятие у преподавателя
+ **/list** - показывает расписание преподавателя на целый день
+ **/link** - вывод ссылки на сайт с графиком занятий преподавателя


## Работа бота на скриншотах:
<img src="https://github.com/user-attachments/assets/49da0a7a-1f99-4dfa-9019-069da12f992f" width="300" height="275">

<img src="https://github.com/user-attachments/assets/65ddad52-135f-485b-ae4b-1805f45acdf7" width="300" height="275">

<img src="https://github.com/user-attachments/assets/cbf451bd-5662-4ad3-8752-50d372c8c2cf" width="300" height="275">

<img src="https://github.com/user-attachments/assets/ad79b8ef-d9b7-44a0-9add-c2c3ac171e52" width="300" height="275">

## Особенности
**FindToTeacher запоминает каждую команду, которую ввел пользователь.** Таким образом нет необходимости при каждом запросе прописывать конкретную команду, что позволяет изначально настроить удобный формат вывода данных под себя. Команды хранятся в переменной, содержащей пары (ID, Command). Это не дает произойти нарушениям работы бота из-за перезаписи команд сторонними пользователями.

При использовании бота учитывайте, что **бот чувствителен к регистру**. Это означает, что при записи "Ткаценко и.С." выдаст ошибку, поскольку инициал "и" не заглавный.

**FindToTeacher и взаимодействие с пользователем.** В воскресные дни, а также при запросе в позднее время (когда заканчивается последняя лекция) бот будет сообщать пользователю, что сейчас ни у кого нет занятий и все заслуженно отдыхают.
