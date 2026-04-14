1.1 Endpoints Reference (endpoints.md)
Auth
Метод	URL	Описание
POST	/api/Auth/register	Регистрация нового пользователя
POST	/api/Auth/login	Логин и получение JWT токена

Пример запроса POST /api/Auth/register:

{
  "email": "alex2022@example.com",
  "password": "Password123!",
  "firstName": "Alex",
  "lastName": "Black"
}


Пример ответа 200 OK:

{
  "email": "alex2022@example.com",
  "firstName": "Alex",
  "lastName": "Black"
}

Users
Метод	URL	Описание
GET	/api/Users/me	Получение профиля текущего пользователя
PUT	/api/Users/me	Обновление профиля пользователя
DELETE	/api/Users/me	Удаление аккаунта пользователя

Пример GET /api/Users/me:

{
  "firstName": "Alex",
  "lastName": "Black",
  "email": "alex2022@example.com"
}

AI
Метод	URL	                              Описание
POST	/api/ai/analyze	                  Анализ изображения AI
POST	/api/ai/ask/{sessionId}	          Задать вопрос AI по конкретной сессии
POST	/api/ai/feedback/{sessionId}	  Отправка обратной связи

Пример запроса POST /api/ai/analyze (cURL):

curl -X POST "https://api.example.com/api/ai/analyze" \
-H "Authorization: Bearer <TOKEN>" \
-F "image=@/path/to/image.jpg"


Пример успешного ответа:

{
  "animal": {
    "name": "Tiger",
    "description": "Big cat native to Asia",
    "habitat": "Forest",
    "dangerLevel": "High",
    "rarityLevel": "Endangered"
  },
  "technical": {
    "usage": {
      "queue_time": 0.5,
      "prompt_tokens": 50,
      "completion_tokens": 100,
      "total_tokens": 150,
      "total_time": 1.2
    }
  },
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
