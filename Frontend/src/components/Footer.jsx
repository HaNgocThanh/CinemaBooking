import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';

/**
 * Footer Component - Premium Footer Design
 * Semantic HTML with smooth interactions
 * 150px fixed margins, gradient divider
 */
export default function Footer() {
  const linkVariants = {
    hidden: { opacity: 0 },
    visible: { opacity: 1, transition: { duration: 0.6 } },
  };

  const footerLinks = {
    product: [
      { label: 'Lịch chiếu', href: '/showtimes' },
      { label: 'Phim đang chiếu', href: '/movies' },
      { label: 'Danh sách rạp', href: '/cinemas' },
      { label: 'Khuyến mãi', href: '/promotions' },
    ],
    support: [
      { label: 'Câu hỏi thường gặp', href: '/faq' },
      { label: 'Liên hệ', href: '/contact' },
      { label: 'Chính sách bảo mật', href: '/privacy' },
      { label: 'Điều khoản sử dụng', href: '/terms' },
    ],
  };

  const FooterLink = ({ href, children }) => (
    <motion.a
      href={href}
      whileHover={{ x: 4, color: '#f1f5f9' }}
      className="text-sm text-slate-400 hover:text-slate-100 transition-colors duration-300"
    >
      {children}
    </motion.a>
  );

  return (
    <footer className="relative w-full bg-gradient-to-b from-slate-950 to-black border-t border-rose-600/10">
      {/* Gradient Separator */}
      <div className="absolute top-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-rose-600/30 to-transparent" />

      <div className="px-6 sm:px-12 lg:px-[150px] py-20">
        {/* Main Footer Content */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-12 mb-12">
          {/* Brand Column */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            viewport={{ once: true }}
          >
            <Link to="/" className="flex items-center space-x-3 mb-4 group">
              <div className="w-9 h-9 bg-gradient-to-br from-rose-600 to-rose-700 rounded-lg flex items-center justify-center">
                <svg
                  className="w-5 h-5 text-white"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path d="M4 2a2 2 0 00-2 2v12a2 2 0 002 2h12a2 2 0 002-2V4a2 2 0 00-2-2H4z" />
                </svg>
              </div>
              <span className="text-base font-light tracking-wider text-slate-100">
                Cinema<span className="font-semibold text-rose-600">Booking</span>
              </span>
            </Link>
            <p className="text-slate-400 text-sm leading-relaxed">
              Nền tảng đặt vé xem phim trực tuyến hàng đầu Việt Nam
            </p>
          </motion.div>

          {/* Product Links */}
          <motion.div
            variants={linkVariants}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
          >
            <h3 className="text-white font-light text-sm uppercase tracking-widest mb-6">
              Sản phẩm
            </h3>
            <nav className="space-y-3">
              {footerLinks.product.map((link) => (
                <FooterLink key={link.href} href={link.href}>
                  {link.label}
                </FooterLink>
              ))}
            </nav>
          </motion.div>

          {/* Support Links */}
          <motion.div
            variants={linkVariants}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            transition={{ delay: 0.1 }}
          >
            <h3 className="text-white font-light text-sm uppercase tracking-widest mb-6">
              Hỗ trợ
            </h3>
            <nav className="space-y-3">
              {footerLinks.support.map((link) => (
                <FooterLink key={link.href} href={link.href}>
                  {link.label}
                </FooterLink>
              ))}
            </nav>
          </motion.div>

          {/* Contact Info */}
          <motion.div
            variants={linkVariants}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            transition={{ delay: 0.2 }}
          >
            <h3 className="text-white font-light text-sm uppercase tracking-widest mb-6">
              Liên hệ
            </h3>
            <div className="space-y-4">
              <p className="text-slate-400 text-sm">
                <span className="text-white">Thành phố:</span>
                <br />
                Hồ Chí Minh, Việt Nam
              </p>
              <p className="text-slate-400 text-sm">
                <span className="text-white">Email:</span>
                <br />
                <a
                  href="mailto:support@cinemabooking.com"
                  className="text-rose-500 hover:text-rose-400 transition-colors"
                >
                  support@cinemabooking.com
                </a>
              </p>
              <p className="text-slate-400 text-sm">
                <span className="text-white">Hotline:</span>
                <br />
                <a
                  href="tel:0929558395"
                  className="text-rose-500 hover:text-rose-400 transition-colors"
                >
                  0929 558 395
                </a>
              </p>
            </div>
          </motion.div>
        </div>

        {/* Divider */}
        <div className="border-t border-slate-800/50 pt-8 mt-8" />

        {/* Bottom Section */}
        <motion.div
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          transition={{ duration: 0.6, delay: 0.3 }}
          viewport={{ once: true }}
          className="flex flex-col lg:flex-row justify-between items-center gap-6"
        >
          <p className="text-slate-500 text-sm">
            © 2026 CinemaBooking. All rights reserved.
          </p>
          <div className="flex gap-6">
            {[
              {
                name: 'Facebook',
                icon: 'M19 3a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h14m-.5 15.5v-5.3a3.26 3.26 0 0 0-3.26-3.26c-.85 0-1.84.52-2.32 1.39v-1.2h-2.84v8.37h2.84v-4.93c0-.77.62-1.4 1.39-1.4a1.4 1.4 0 0 1 1.4 1.4v4.93h2.84M6.88 8.56a1.68 1.68 0 0 0 1.68-1.68c0-.93-.75-1.69-1.68-1.69a1.69 1.69 0 0 0-1.69 1.69c0 .93.76 1.68 1.69 1.68m1.39 9.94h-2.78v-8.37h2.78v8.37Z',
              },
              {
                name: 'Twitter',
                icon: 'M22.46 6c-.87.39-1.8.65-2.77.77 1-.6 1.76-1.55 2.12-2.68-.94.55-1.98.95-3.08 1.17-.89-.95-2.16-1.54-3.57-1.54-2.7 0-4.88 2.19-4.88 4.88 0 .38.04.75.14 1.1-4.06-.2-7.67-2.15-10.08-5.1-.42.73-.66 1.58-.66 2.48 0 1.7.86 3.2 2.17 4.08-.8-.03-1.56-.25-2.22-.62v.06c0 2.36 1.68 4.35 3.91 4.8-.41.11-.84.17-1.28.17-.31 0-.62-.03-.93-.08.63 1.93 2.43 3.34 4.57 3.38-1.67 1.3-3.77 2.08-6.05 2.08-.39 0-.77-.02-1.15-.08 2.17 1.39 4.74 2.2 7.55 2.2 9.05 0 13.99-7.5 13.99-13.99 0-.21 0-.42-.01-.62.96-.69 1.79-1.56 2.45-2.55z',
              },
              {
                name: 'YouTube',
                icon: 'M23.498 6.186a3.016 3.016 0 0 0-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 0 0 .502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 0 0 2.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 0 0 2.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814zM9.545 15.568V8.432L15.818 12l-6.273 3.568z',
              },
            ].map(({ name, icon }) => (
              <motion.a
                key={name}
                href="#"
                whileHover={{ scale: 1.2, color: '#e11d48' }}
                className="text-slate-400 hover:text-rose-600 transition-colors"
                aria-label={name}
              >
                <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                  <path d={icon} />
                </svg>
              </motion.a>
            ))}
          </div>
        </motion.div>
      </div>
    </footer>
  );
}
