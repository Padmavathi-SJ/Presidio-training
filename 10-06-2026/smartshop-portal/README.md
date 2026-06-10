SmartShop Portal

An Angular e-commerce application demonstrating routing, API integration, RxJS, and
modern Angular features.

Features
- Authentication - Login with DummyJSON API
- Products - Browse products with grid layout
- Product Details - View product images, ratings, and descriptions
- User Profile - View logged-in user information
- Route Protection - AuthGuard for protected routes
- RxJS - BehaviorSubject, tap, map, and catchError operators
- Signals - Modern Angular reactive state management

Prerequisites
- Node.js (v18 or later)
- npm (v9 or later)

Installation
1. Clone or download the project
2. Install dependencies
npm install

Running the Application
Development Server
ng serve

Navigate to `http://localhost:4200`

Login Credentials
Use these demo credentials to log in:
- Username: `emilys`
- Password: `emilyspass`

Project Structure
src/app/
├── login/ # Login component
├── dashboard/ # Dashboard with navigation
├── products/ # Products listing (Signals)
├── product-details/ # Product details (Signals)
├── profile/ # User profile
├── header/ # Header with user salutation
├── services/ # AuthService, ProductService
├── guards/ # AuthGuard
└── models/ # TypeScript interfaces

API Endpoints
- Login: `POST https://dummyjson.com/auth/login`
- Products: `GET https://dummyjson.com/products`
- Product by ID: `GET https://dummyjson.com/products/:id`