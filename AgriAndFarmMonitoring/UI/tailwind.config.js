// tailwind.config.js
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
    "./src/**/*.component.{html,ts}",
    "./src/app/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        // Earthy Greens - Primary
        primary: {
          50: '#e8f5e9',
          100: '#c8e6c9',
          200: '#a5d6a7',
          300: '#81c784',
          400: '#66bb6a',
          500: '#4caf50',
          600: '#40916c',
          700: '#2d6a4f',
          800: '#1b4332',
          900: '#0f5238',
          DEFAULT: '#2d6a4f',
        },
        // Warm Clay - Secondary
        secondary: {
          50: '#fdf6ee',
          100: '#fce8d5',
          200: '#f9d1ab',
          300: '#f5b980',
          400: '#d4a373',
          500: '#c9935e',
          600: '#b0784d',
          700: '#8a5d3b',
          800: '#7d562d',
          900: '#623f18',
          DEFAULT: '#d4a373',
        },
        // Tonal Layers
        surface: {
          DEFAULT: '#f8f9fa',
          dim: '#d9dadb',
          bright: '#f8f9fa',
          container: {
            lowest: '#ffffff',
            low: '#f3f4f5',
            DEFAULT: '#edeeef',
            high: '#e7e8e9',
            highest: '#e1e3e4',
          }
        },
        'on-surface': {
          DEFAULT: '#191c1d',
          variant: '#404943',
        },
        inverse: {
          surface: '#2e3132',
          'on-surface': '#f0f1f2',
          primary: '#95d4b3',
        },
        outline: {
          DEFAULT: '#707973',
          variant: '#bfc9c1',
        },
        // Semantic Colors
        error: {
          DEFAULT: '#ba1a1a',
          container: '#ffdad6',
          'on-container': '#93000a',
        },
        tertiary: {
          DEFAULT: '#005236',
          container: '#006d48',
          'on-container': '#89edba',
          fixed: '#92f7c3',
          'fixed-dim': '#75daa8',
        },
        // Status Colors
        success: '#2d6a4f',
        warning: '#d4a373',
        danger: '#ba1a1a',
        info: '#40916c',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
      },
      fontSize: {
        'display-lg': ['48px', { lineHeight: '56px', letterSpacing: '-0.02em', fontWeight: '700' }],
        'headline-lg': ['32px', { lineHeight: '40px', letterSpacing: '-0.01em', fontWeight: '600' }],
        'headline-lg-mobile': ['24px', { lineHeight: '32px', fontWeight: '600' }],
        'headline-md': ['24px', { lineHeight: '32px', fontWeight: '600' }],
        'body-lg': ['18px', { lineHeight: '28px', fontWeight: '400' }],
        'body-md': ['16px', { lineHeight: '24px', fontWeight: '400' }],
        'body-sm': ['14px', { lineHeight: '20px', fontWeight: '400' }],
        'label-md': ['12px', { lineHeight: '16px', letterSpacing: '0.05em', fontWeight: '600' }],
        'data-tabular': ['14px', { lineHeight: '20px', fontWeight: '500' }],
      },
      borderRadius: {
        'sm': '0.25rem',
        'DEFAULT': '0.5rem',
        'md': '0.75rem',
        'lg': '1rem',
        'xl': '1.5rem',
        'full': '9999px',
      },
      spacing: {
        'unit': '8px',
        'gutter': '24px',
        'margin-mobile': '16px',
        'margin-desktop': '32px',
        'sidebar': '280px',
      },
      maxWidth: {
        'container': '1440px',
      },
      boxShadow: {
        'card': '0px 4px 12px rgba(45, 106, 79, 0.08)',
        'modal': '0px 8px 24px rgba(0, 0, 0, 0.12)',
        'dropdown': '0px 4px 16px rgba(0, 0, 0, 0.10)',
      },
    },
  },
  plugins: [],
}