AiQuestionDto
{
  "questionAboutNature": "What is the largest animal in the world?"
}

AiFeedbackDto
{
  "rating": 100,
  "comment": "AI correctly recognized the image"
}

LoginUserDto
{
  "email": "alex2022@example.com",
  "password": "Password123!"
}

RegisterUserDto
{
  "email": "alex2022@example.com",
  "password": "Password123!",
  "firstName": "Alex",
  "lastName": "Black"
}

UpdateUserDto
{
  "firstName": "Alex",
  "lastName": "Black",
  "email": "alex2022@example.com"
}

AnimalAnalysisResponseDto
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
