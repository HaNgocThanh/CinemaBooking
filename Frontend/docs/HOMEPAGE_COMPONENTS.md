# 🎬 Cinema Booking - Home Page Components Documentation

## 📋 Tổng quan

Tài liệu này mô tả các component được tạo lại để trang chủ của Website Đặt vé phim, tuân thủ theo **Clean Architecture** và **Frontend Conventions**.

---

## 🏗️ Cấu trúc Component

```
src/Frontend/src/
├── components/
│   ├── Navbar.jsx          # Thanh điều hướng sticky
│   ├── Footer.jsx          # Phần footer với links
│   └── index.js            # Xuất tất cả components
│
├── features/movies/
│   └── components/
│       ├── MovieCard.jsx   # Card phim với hiệu ứng hover
│       ├── HeroSection.jsx # Hero banner với phim nổi bật
│       ├── MovieGrid.jsx   # Lưới phim responsive
│       └── index.js        # Xuất tất cả components
│
└── pages/
    └── HomePage.jsx        # Trang chủ chính
```

---

## 🎨 Thành phần & Chức năng

### 1. **Navbar** (`components/Navbar.jsx`)
Thanh điều hướng sticky với dark theme.

**Props:** Không có props
**Tính năng:**
- Logo "CinemaBooking" với icon ticket
- Menu desktop (Lịch chiếu, Rạp, Ưu đãi)
- Nút "Đăng nhập" (ghost style)
- Menu mobile responsive
- Backdrop blur effect (bg-slate-950/80)
- Sticky top-0, z-50

**Ví dụ sử dụng:**
```jsx
import Navbar from '@/components/Navbar';

function MyPage() {
  return <Navbar />;
}
```

---

### 2. **MovieCard** (`features/movies/components/MovieCard.jsx`)
Component card phim tái tạo hiệu ứng hover từ Figma.

**Props:**
```typescript
interface MovieCardProps {
  movie: {
    id: string | number;
    title: string;
    posterUrl: string;
  };
  onBuyTicket?: (movieId) => void;    // Callback mua vé
  onWatchTrailer?: (movieId) => void;  // Callback xem trailer
}
```

**Tính năng:**
- Kích thước: Tỷ lệ 2:3 (responsive)
- Bo góc: 16px (rounded-2xl)
- **Hiệu ứng Hover:**
  - Ảnh phóng to 1.05x (scale-105) trong 300ms
  - Overlay tối mờ với backdrop-blur
  - Hai nút CTA: "Mua vé" (Rose-600) & "Trailer" (Ghost)
  - Smooth transition tất cả

**Ví dụ sử dụng:**
```jsx
import { MovieCard } from '@/features/movies/components';

const movie = {
  id: 1,
  title: 'Inception',
  posterUrl: 'https://...'
};

function MovieList() {
  return (
    <MovieCard
      movie={movie}
      onBuyTicket={(id) => navigate(`/showtimes?movie=${id}`)}
      onWatchTrailer={(id) => openTrailerModal(id)}
    />
  );
}
```

---

### 3. **HeroSection** (`features/movies/components/HeroSection.jsx`)
Banner hero lớn với phim nổi bật.

**Props:**
```typescript
interface HeroSectionProps {
  featuredMovie: {
    id: number;
    title: string;
    description: string;
    duration: number;
    releaseYear: number;
    rating: number;
    backdropUrl: string;
  };
}
```

**Tính năng:**
- Ảnh nền tràn viền (backdrop-url)
- Gradient overlay (from-slate-950 → transparent)
- Badge "NOW SHOWING"
- H1 lớn (64px bold)
- Metadata (duration, year, rating)
- Description (max-width, line-clamp-3)
- CTA buttons (Fill + Ghost style)

**Ví dụ sử dụng:**
```jsx
import HeroSection from '@/features/movies/components/HeroSection';

const featured = {
  id: 1,
  title: 'Dune: Part Two',
  description: 'Khải chiến trong tập phim này',
  duration: 166,
  releaseYear: 2024,
  rating: 8.5,
  backdropUrl: 'https://...'
};

<HeroSection featuredMovie={featured} />
```

---

### 4. **MovieGrid** (`features/movies/components/MovieGrid.jsx`)
Lưới card phim responsive.

**Props:**
```typescript
interface MovieGridProps {
  movies: Array<{
    id: string | number;
    title: string;
    posterUrl: string;
  }>;
  title?: string;              // Tiêu đề section (mặc định: "Phim đang chiếu")
  onBuyTicket?: (id) => void;  
  onWatchTrailer?: (id) => void;
}
```

**Tính năng:**
- Responsive grid: 2 cột (mobile) → 3 cột (tablet) → 4 cột (desktop)
- Tiêu đề section với underline Rose-600
- Hiển thị từng MovieCard
- Nút "Xem thêm" khi có ≥8 phim

**Responsive Breakpoints:**
```
- Mobile (sm): 2 columns
- Tablet (md): 3 columns
- Desktop (lg): 4 columns
```

**Ví dụ sử dụng:**
```jsx
import MovieGrid from '@/features/movies/components/MovieGrid';

const movies = [
  { id: 1, title: 'Movie 1', posterUrl: '...' },
  { id: 2, title: 'Movie 2', posterUrl: '...' },
  // ... 6+ more movies
];

<MovieGrid
  movies={movies}
  title="Phim đang chiếu"
  onBuyTicket={(id) => handleBuy(id)}
  onWatchTrailer={(id) => handleTrailer(id)}
/>
```

---

### 5. **Footer** (`components/Footer.jsx`)
Phần footer với links, contact info, và social media.

**Props:** Không có props

**Tính năng:**
- 4 cột: Brand, Product, Support, Contact
- Links hợp lý (Showtimes, Movies, Cinemas, Promotions, FAQ, Privacy)
- Contact info (Email, Phone, Address)
- Social media icons (Facebook, Twitter, YouTube)
- Copyright & bottom links
- Dark theme, semantic HTML

**Ví dụ sử dụng:**
```jsx
import Footer from '@/components/Footer';

function Layout() {
  return (
    <>
      <main>...</main>
      <Footer />
    </>
  );
}
```

---

### 6. **HomePage** (`pages/HomePage.jsx`)
Trang chủ chính kết hợp tất cả component.

**Props:** Không có props (Page component)

**Cấu trúc:**
1. **Navbar** - Sticky navigation
2. **HeroSection** - Featured movie banner
3. **Main Content**
   - MovieGrid "Phim đang chiếu" (8 phim)
   - MovieGrid "Phim sắp tới" (4 phim)
4. **Footer** - Footer section

**Dữ liệu Mẫu:**
- Dữ liệu mock movies hiện tại dùng placeholder
- Có thể thay bằng API call (useQuery từ React Query)

**Ví dụ tích hợp React Query:**
```jsx
import { useQuery } from '@tanstack/react-query';
import axiosClient from '@/services/axiosClient';

export default function HomePage() {
  // Fetch now showing movies
  const { data: nowShowing = [] } = useQuery(
    'moviesNowShowing',
    () => axiosClient.get('/api/movies/now-showing')
  );

  // Fetch coming soon movies
  const { data: comingSoon = [] } = useQuery(
    'moviesComingSoon',
    () => axiosClient.get('/api/movies/coming-soon')
  );

  // ... rest of component
}
```

---

## 🎨 Bảng Màu (Color Palette)

Áp dụng **Dark Theme** đã thiết kế:

| Element | Color | Tailwind Class |
|---------|-------|----------------|
| Nền chính | #0F172A | bg-slate-950 |
| Nền Surface | #1E293B | bg-slate-900 |
| Primary (CTA) | #E11D48 | bg-rose-600 |
| Chữ chính | #F8FAFC | text-slate-100 |
| Chữ phụ | #94A3B8 | text-slate-400 |
| Border | #475569 | border-slate-600 |

---

## 📏 Typography

| Element | Size | Weight | Notes |
|---------|------|--------|-------|
| H1 (Hero) | 64px (md: 56px) | bold (700) | Lớn, nổi bật |
| H2 (Section) | 36px (md: 32px) | bold | Tiêu đề section |
| Body | 16px | normal (400) | Văn bản thường |
| Small | 14px | normal | Metadata, hints |
| Badge | 12px | semibold | Badge "NOW SHOWING" |

---

## ✨ Hiệu ứng & Transitions

### MovieCard Hover
```
Duration: 300ms
Easing: ease-out
Image Scale: 100% → 105%
Overlay Opacity: 0% → 100%
Buttons: Fade in, scale-up animation
```

### Button Interactions
```
Hover: bg-color-darker, scale-105
Active: scale-95 (press-down effect)
Transition: 200ms duration
```

### Navbar
```
Position: sticky top-0
Backdrop: blur-md, bg-slate-950/80
z-index: 50 (above content)
```

---

## 🔗 Routing & Navigation

### Routes được định nghĩa:
- `/` → HomePage (full-width, custom Navbar/Footer)
- `/login` → LoginPage
- `/showtimes?movie={id}` → Showtime selection
- `/cinemas` → Cinema listing
- `/promotions` → Promotions page

**Navigate từ MovieCard:**
```jsx
import { useNavigate } from 'react-router-dom';

const handleBuyTicket = (movieId) => {
  navigate(`/showtimes?movie=${movieId}`);
};
```

---

## 🧪 Testing Tips

1. **Hover Effects:** Hover over MovieCard để thấy transition mượt mà
2. **Responsive:** Test trên mobile (320px), tablet (768px), desktop (1024px)
3. **Accessibility:** Tất cả buttons/links có `aria-label`
4. **Colors:** Kiểm tra dark theme colors đúng theo Figma

---

## 📝 Coding Standards

✅ **Clean Code Principles:**
- Semantic HTML (`<nav>`, `<article>`, `<footer>`, `<button>`)
- Component naming: PascalCase (MovieCard, HeroSection)
- Props validation: PropTypes
- No magic numbers - use Tailwind classes
- Comments cho components phức tạp

✅ **Tailwind Usage:**
- Dùng Tailwind classes thay vì inline CSS
- Responsive classes: `sm:`, `md:`, `lg:`
- Dark mode ready: tất cả colors từ slate palette
- Group utilities: `.group` hover effects

✅ **Performance:**
- Memo components nếu cần (optional)
- Lazy load images khi cần
- Use `img` tag với `alt` attributes
- Optimize bundle size

---

## 🚀 Next Steps

1. **Integrate React Query** - Thay mock data bằng API calls
2. **Add Trailer Modal** - Click "Trailer" button
3. **Search & Filter** - Search box ở Navbar
4. **User Authentication** - Login/Register flow
5. **Booking Flow** - Seat selection, payment

---

## 📞 Support & Questions

Để mở rộng hoặc chỉnh sửa các component, tham khảo:
- [docs/architecture.md](../architecture.md) - Clean Architecture
- [docs/frontend-conventions.md](../frontend-conventions.md) - Quy tắc frontend
- [docs/api-patterns.md](../api-patterns.md) - API integration patterns
