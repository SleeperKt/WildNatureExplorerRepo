2. Responses Examples
2.1 Auth
Register - Incorrect Email Format
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "Email format is invalid."
    ]
  },
  "traceId": "00-07641bb59e4781a773147387ed61ec39-e963bbd0b314fcdd-00"
}

Register - Incorrect Password Format
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Password": [
      "Password must contain at least one uppercase letter."
    ]
  },
  "traceId": "00-714f6c536c9248cbb60c0569d84dd590-1325a1af266c8727-00"
}

Register - Correct Format
{
  "id": "07407741-5d4e-4991-a076-70dcf28f02cf",
  "email": "example@gmail.com",
  "firstName": "Alex",
  "lastName": "Black",
  "isActive": true
}

Login - Incorrect Credentials
{
  "type": "error",
  "title": "An unexpected error occurred",
  "status": 500
}

Login - Correct Credentials
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}

2.2 Users
Get Me - Success
{
  "firstName": "Alex",
  "lastName": "Black",
  "email": "example@gmail.com"
}

Get Me - Unauthorized
{
  "type": "error",
  "title": "Unauthorized",
  "status": 401
}

Update Profile - Success
{}


(No content, 204 No Content)

Update Profile - Invalid Email
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "Email format is invalid."
    ]
  },
  "traceId": "00-abc123-example-trace"
}

Delete Account - Success
{}


(No content, 204 No Content)

2.3 AI
Analyze Image - Success
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

Analyze Image - Unauthorized
{
  "type": "error",
  "title": "Unauthorized",
  "status": 401
}

Ask AI - Correct
{
  "answer": "The largest animal in the world is the Blue Whale.",
  "technical": {
    "usage": {
      "queue_time": 0.3,
      "prompt_tokens": 12,
      "completion_tokens": 15,
      "total_tokens": 27,
      "total_time": 0.5
    }
  }
}

Ask AI - Invalid Question
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "QuestionAboutNature": [
      "Question must be at least 8 characters and end with a '?'."
    ]
  },
  "traceId": "00-example-trace"
}

Feedback AI - Correct
{}


(No content, 200 OK)

Feedback AI - Invalid Rating
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Rating": [
      "Rating must be between 1 and 100."
    ]
  },
  "traceId": "00-example-trace"
}

2.4 Health Check
Health Check - Success
{}


(200 OK, empty body)

Health Check - Example Failure
{
  "type": "error",
  "title": "Service unavailable",
  "status": 503
}