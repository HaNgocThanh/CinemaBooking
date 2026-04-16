# Cinema Booking System - Frontend

A modern React + Vite frontend for the Cinema Booking System with comprehensive state management using React Query, form validation with React Hook Form, and styling with Tailwind CSS and Ant Design.

## 🚀 Features

- **React 18** with Vite for fast development
- **React Router** for client-side routing
- **React Query (TanStack Query)** for server state management
- **Axios** with interceptors for API communication
- **React Hook Form** + **Zod** for form validation
- **Tailwind CSS** for utility-first styling
- **Ant Design** for admin UI components
- **Lucide React** for beautiful icons
- **Feature Slices Architecture** for scalable structure

## 📁 Project Structure

```
frontend/
├── src/
│   ├── assets/              # Static assets
│   ├── components/          # Shared presentational components
│   │   ├── LoadingSpinner.jsx
│   │   ├── ErrorCard.jsx
│   │   └── index.js
│   ├── config/              # Configuration files
│   │   └── queryClient.js   # React Query setup
│   ├── features/            # Feature-based modules
│   │   ├── auth/
│   │   │   ├── components/
│   │   │   ├── hooks/
│   │   │   ├── services/
│   │   │   │   └── authApi.js
│   │   │   └── AuthPage.jsx
│   │   ├── movies/
│   │   │   ├── components/
│   │   │   │   └── MovieCard.jsx
│   │   │   ├── hooks/
│   │   │   ├── services/
│   │   │   │   └── movieApi.js
│   │   │   └── MoviesPage.jsx
│   │   └── bookings/        # Coming soon
│   ├── hooks/               # Custom hooks
│   │   ├── useCountdown.js
│   │   └── index.js
│   ├── layouts/             # Layout components
│   │   ├── MainLayout.jsx
│   │   └── index.js
│   ├── services/            # Shared services
│   │   └── axiosClient.js   # Axios instance with interceptors
│   ├── utils/               # Utility functions
│   ├── App.jsx              # Main app component
│   ├── main.jsx             # Entry point
│   ├── index.css            # Global styles with Tailwind
│   └── App.css
├── .env.example             # Environment variables template
├── .env.local               # Local environment variables
├── package.json
├── tailwind.config.js       # Tailwind CSS configuration
├── postcss.config.js        # PostCSS configuration
├── vite.config.js           # Vite configuration
└── README.md
```

## 🏗️ Architecture - Feature Slices

Each feature is organized in its own folder with a clear separation of concerns:

```
src/features/auth/
├── components/              # UI components (presentational only)
│   └── LoginForm.jsx
├── hooks/                   # Custom hooks
│   └── useLoginForm.js
├── services/                # API integration with React Query
│   └── authApi.js
├── AuthPage.jsx             # Feature container/page
└── index.js                 # Export public API
```

### Rules:

- **components/** = Pure presentational components (NO hooks, NO API calls)
- **hooks/** = Custom hooks (business logic)
- **services/** = API queries using React Query
- Feature page uses hooks and components to build the feature

## 🔧 Setup & Installation

### Prerequisites

- Node.js 16+
- npm or yarn

### Steps

1. **Navigate to frontend directory:**
   ```bash
   cd d:\CinemaBooking\frontend
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Configure environment variables:**
   ```bash
   cp .env.example .env.local
   # Edit .env.local with your API URL
   ```

4. **Start development server:**
   ```bash
   npm run dev
   ```

5. **Build for production:**
   ```bash
   npm run build
   ```

## 📦 Dependencies

### Core Libraries

- **react** (18.x) - UI framework
- **react-dom** - React DOM rendering
- **react-router-dom** - Client-side routing
- **vite** - Build tool & dev server

### State Management & API

- **@tanstack/react-query** - Server state management
- **axios** - HTTP client

### Forms & Validation

- **react-hook-form** - Form state management
- **zod** - Schema validation
- **@hookform/resolvers** - Zod integration with React Hook Form

### UI & Styling

- **tailwindcss** - Utility-first CSS
- **antd** - Enterprise-grade UI components
- **lucide-react** - Icon library
- **postcss** - CSS transformations
- **autoprefixer** - Vendor prefixes

## 🚀 Development Workflow

### Creating a New Feature

1. **Create feature folder:**
   ```bash
   mkdir -p src/features/myfeature/{components,hooks,services}
   ```

2. **Create API integration** in `services/myFeatureApi.js`:
   ```javascript
   import { useQuery, useMutation } from '@tanstack/react-query';
   import axiosClient from '../../../services/axiosClient';

   export function useMyData() {
     return useQuery({
       queryKey: ['mydata'],
       queryFn: () => axiosClient.get('/myendpoint'),
     });
   }
   ```

3. **Create presentational components** in `components/`:
   ```javascript
   // ✅ No hooks, no API calls
   export function MyComponent({ data, onAction }) {
     return <div>{data}</div>;
   }
   ```

4. **Create feature page** `MyFeaturePage.jsx`:
   ```javascript
   import { useMyData } from './services/myFeatureApi';
   import { MyComponent } from './components/MyComponent';
   
   export function MyFeaturePage() {
     const { data, isLoading, error } = useMyData();
     
     if (isLoading) return <LoadingSpinner />;
     if (error) return <ErrorCard message={error.message} />;
     
     return <MyComponent data={data} />;
   }
   ```

5. **Add route** in `App.jsx`:
   ```javascript
   <Route path="/myfeature" element={<MyFeaturePage />} />
   ```

## 🔌 API Integration

### Axios Client

The `axiosClient.js` handles:
- Base URL configuration
- JWT token injection in headers
- Response error handling
- Automatic redirect on 401 (token expiry)

```javascript
// Usage in React Query
const { data } = useQuery({
  queryKey: ['movies'],
  queryFn: () => axiosClient.get('/movies'),
});
```

### React Query

- All API calls go through React Query
- Automatic caching and refetching
- Real-time updates with `refetchInterval`
- Mutation with `onSuccess` and `onError` callbacks

Example - Polling seats every 5 seconds:
```javascript
export function useSeats(showtimeId) {
  return useQuery({
    queryKey: ['seats', showtimeId],
    queryFn: () => axiosClient.get(`/showtimes/${showtimeId}/seats`),
    staleTime: 1000,
    refetchInterval: 5000, // Poll every 5 seconds
  });
}
```

## 📋 Form Validation

Using **React Hook Form** + **Zod** for type-safe validation:

```javascript
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';

const loginSchema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(6, 'Min 6 characters'),
});

export function LoginForm() {
  const { register, handleSubmit, formState: { errors } } = useForm({
    resolver: zodResolver(loginSchema),
  });

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <input {...register('email')} />
      {errors.email && <span>{errors.email.message}</span>}
    </form>
  );
}
```

## 🎨 Styling with Tailwind CSS

Mobile-first responsive design:

```jsx
<div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
  {/* 2 cols on mobile, 3 on tablet, 4 on desktop */}
</div>
```

Custom colors configured in `tailwind.config.js`:
- `primary` - #1890ff (Ant Design blue)
- `secondary` - #722ed1
- `danger` - #ff4d4f
- `success` - #52c41a
- `warning` - #faad14

## 🧪 Custom Hooks

### useCountdown

5-minute countdown timer for seat selection:

```javascript
const { formattedTime, isExpired } = useCountdown(300);

return (
  <div className={isExpired ? 'text-danger' : ''}>
    Time: {formattedTime}
  </div>
);
```

## 📱 Responsive Design

- Mobile-first approach
- Breakpoints: `sm` (640px), `md` (768px), `lg` (1024px), `xl` (1280px)
- Flexible layouts with Tailwind Grid and Flexbox

## 🔐 Environment Variables

Create `.env.local` file (copy from `.env.example`):

```
VITE_API_URL=http://localhost:5000/api
VITE_ENV=development
```

## 📚 Useful Commands

```bash
# Development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Check for linting (if configured)
npm run lint
```

## 🤝 Contributing

When adding new features:

1. Follow Feature Slices architecture
2. Keep components presentational (no hooks/API)
3. Use React Query for all API calls
4. Use React Hook Form for forms
5. Use Tailwind for styling
6. Add proper TypeScript types (when migrating to TS)

## 📖 Resources

- [React Documentation](https://react.dev)
- [React Router](https://reactrouter.com)
- [TanStack Query](https://tanstack.com/query)
- [Tailwind CSS](https://tailwindcss.com)
- [Ant Design](https://ant.design)
- [React Hook Form](https://react-hook-form.com)
- [Zod](https://zod.dev)

## 📄 License

Part of Cinema Booking System Project
