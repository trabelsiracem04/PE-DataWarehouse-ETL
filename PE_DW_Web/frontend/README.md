# DW Analytics — Frontend

React 19 + TypeScript SPA for the DW Analytics BI dashboard.

## Tech Stack

- React 19 + TypeScript
- Vite 8 (build tool)
- Recharts (charting)
- chroma-js (color scales)
- react-router-dom (routing)

## Available Scripts

```bash
npm run dev      # Start dev server on http://localhost:5173
npm run build    # Production build to dist/
npm run preview  # Preview production build
npx tsc -b       # TypeScript type-check
```

## Project Structure

```
src/
├── api/           # API client functions (dashboardApi, reportApi)
├── components/    # Reusable UI: layout, charts, KPI, header
├── pages/         # DashboardPage, ReportsPage
├── types/         # TypeScript interfaces
├── theme/         # Colors, chroma scales
├── App.tsx        # Router setup
├── main.tsx       # Entry point
└── index.css      # Global styles
```

## API

The frontend expects a .NET backend at `http://localhost:5125` by default. Override via `VITE_API_BASE_URL` environment variable.
