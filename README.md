## Overview
This is a demo project created for learning purposes, as it is my first .NET application.

The API serves as the backend for a social media platform, which I plan to integrate into a website or mobile application.

## Table of contents
- [Overview](#overview)
- [Features by routes](#features-by-routes)
- [Services](#services)
- [Usage](#usage)
  - [API Documentation](#api-documentation)
  - [Versioning](#versioning)
  - [Pagination](#pagination)
  - [Caching](#caching)

## Features by routes
### Users
- Create
- Get by ID
- Get all (paged)

### Posts
- Get by ID
- Get all (paged)
- Create
- Edit
- Delete

### Likes of posts
- Like and dislike
- Get likes of post (unpaged)

### Comments of posts
- Add
- Edit
- Delete
- Get comments of post (unpaged)

## Services
- .NET 9 as backend
- PostgreSQL as database
- Redis as cache

## Usage
### API Documentation
All endpoints are documented and accessible via /scalar.

### Versioning
The API is versioned. You must specify the version in the URL (example: /api/v1/users).

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

The cached data sets on GET/POST, resets on PUT/PATCH, and invalidates on DELETE requests.

The default cache time is 10 minutes.