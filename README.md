## Table of contents
- [Overview](#overview)
- [Services](#services)
- [Features by routes](#features-by-routes)
- [Usage](#usage)
    - [API Documentation](#api-documentation)
    - [Versioning](#versioning)
    - [Pagination](#pagination)
    - [Caching](#caching)

## Overview
This is a demo project created for learning purposes, as it is my first .NET application.

The API serves as the backend for a social media platform, which I plan to integrate into a website or mobile application.

## Services
- .NET 9 as backend
- PostgreSQL as database
- Redis as cache
- Nginx as proxy server

## Features by routes
### Auth
- Register
- Login
- Refresh access

### Users
- Get by ID
- Get all (paged)
- #### Friendships
  - Add friend
  - Accept friend request
  - Decline friend request
  - Delete friend
  - Get friends of user (unpaged)
  - Get friend requests of user (unpaged)

### Posts
- Get by ID
- Get all (paged)
- Create
- Edit
- Delete
- #### Likes of posts
  - Like and dislike
  - Get likes of post (unpaged)
- #### Comments of posts
  - Add
  - Edit
  - Delete
  - Get comments of post (unpaged)

## Usage
### API Documentation
All endpoints are documented and accessible via /scalar.

### Versioning
The API is versioned. You must specify the version in the URL (example: /api/v1/users).

### Authentication
The API uses JWT authentication. 
- Tokens are generated on login, register and refresh-token endpoints and must be passed in the Authorization header.
- The token is valid for 15 minutes. 
- The refresh token expiration depends on the RememberMe field in the login request.
    - If RememberMe is true, the expiration time is 14 days.
    - If RememberMe is false, the expiration time is 12 hours.
- The expiration time is a sliding window, so after refreshing, it is valid for another 14 days or 12 hours, until it reaches the maximum expiration time, which is 90 days.
- Token can be refreshed by sending a POST request to /api/v1/auth/refresh-token. Then a new set of tokens is returned (access and refresh).
- If the JWT or Refresh token is invalid for some reason (ex. an older jwt has been sent with the correct refresh token or some claims are invalid or missing),
  then the Token gets invalidated and deleted, so the user must login again.
- GET endpoints do not require authentication, but POST, PUT, PATCH and DELETE do.

### Pagination
When retrieving multiple objects (lists) of an entity root (eg. AppUser or Post), the API returns a paginated response.
To make use of it, you must specify the page number and page size in the query.

Example: /api/v1/users?pageNumber=2&pageSize=5

By default:
- Page size: 10 (means 10 objects per page)
- Page number: 1

Notes:
- The maximum pageSize is 50. If the specified page size is greater than 50, the maximum pageSize is applied.
- If pageNumber and pageSize parameters are not specified in the query, these default values are applied.
- If the pageNumber exceeds the total pages, an empty list is returned.

### Caching
The API uses Redis as cache.

The cached data:
- sets on GET requests which produce single or unpaged results.
- resets on PUT/PATCH requests.
- invalidates on DELETE requests.

Default cache time is 10 minutes.
