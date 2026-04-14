1. Requests Examples
1.1 Auth
Register - Incorrect Email Format
curl -X POST "http://localhost:5000/api/Auth/register" \
-H "accept: */*" \
-H "Content-Type: application/json" \
-d '{
  "email": "q",
  "password": "Qwerty123!",
  "firstName": "Alex",
  "lastName": "Black"
}'

Register - Incorrect Password Format
curl -X POST "http://localhost:5000/api/Auth/register" \
-H "accept: */*" \
-H "Content-Type: application/json" \
-d '{
  "email": "example@gmail.com",
  "password": "qwerty123!",
  "firstName": "Alex",
  "lastName": "Black"
}'

Register - Correct Format
curl -X POST "http://localhost:5000/api/Auth/register" \
-H "accept: */*" \
-H "Content-Type: application/json" \
-d '{
  "email": "example@gmail.com",
  "password": "Aqwerty123!",
  "firstName": "Alex",
  "lastName": "Black"
}'

Login - Incorrect Credentials
curl -X POST "http://localhost:5000/api/Auth/login" \
-H "accept: */*" \
-H "Content-Type: application/json" \
-d '{
  "email": "example@gmail.com",
  "password": "Lqwerty123!"
}'

Login - Correct Credentials
curl -X POST "http://localhost:5000/api/Auth/login" \
-H "accept: */*" \
-H "Content-Type: application/json" \
-d '{
  "email": "example@gmail.com",
  "password": "Aqwerty123!"
}'

1.2 Users
Get Me
curl -X GET "http://localhost:5000/api/Users/me" \
-H "accept: application/json" \
-H "Authorization: Bearer <TOKEN>"

Update Profile - Correct
curl -X PUT "http://localhost:5000/api/Users/me" \
-H "accept: */*" \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <TOKEN>" \
-d '{
  "firstName": "Alex",
  "lastName": "Black",
  "email": "example@gmail.com"
}'

Update Profile - Incorrect Email
curl -X PUT "http://localhost:5000/api/Users/me" \
-H "accept: */*" \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <TOKEN>" \
-d '{
  "firstName": "Alex",
  "lastName": "Black",
  "email": "invalidemail"
}'

Delete Account
curl -X DELETE "http://localhost:5000/api/Users/me" \
-H "accept: */*" \
-H "Authorization: Bearer <TOKEN>"

1.3 AI
Analyze Image
curl -X POST "http://localhost:5000/api/ai/analyze" \
-H "accept: application/json" \
-H "Authorization: Bearer <TOKEN>" \
-F "image=@/path/to/image.jpg"

Ask AI - Correct
curl -X POST "http://localhost:5000/api/ai/ask/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
-H "accept: application/json" \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <TOKEN>" \
-d '{
  "questionAboutNature": "What is the largest animal in the world?"
}'

Ask AI - Invalid Question
curl -X POST "http://localhost:5000/api/ai/ask/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
-H "accept: application/json" \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <TOKEN>" \
-d '{
  "questionAboutNature": "Too short"
}'

Feedback AI - Correct
curl -X POST "http://localhost:5000/api/ai/feedback/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
-H "accept: application/json" \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <TOKEN>" \
-d '{
  "rating": 100,
  "comment": "AI correctly recognized the image"
}'

Feedback AI - Invalid Rating
curl -X POST "http://localhost:5000/api/ai/feedback/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
-H "accept: application/json" \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <TOKEN>" \
-d '{
  "rating": 101,
  "comment": "Invalid rating"
}'

1.4 Health Check
curl -X GET "http://localhost:5000/health" \
-H "accept: application/json"

