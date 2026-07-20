export default function DashboardHeader() {
  return (
    <div className="dashboard-header">
      <h1>Sales Dashboard</h1>
      <div className="filter-bar">
        <select aria-label="Date range">
          <option>Year to Date</option>
          <option>Last 12 Months</option>
          <option>All Time</option>
        </select>
        <select aria-label="Currency">
          <option>All Currencies</option>
        </select>
        <select aria-label="Territory">
          <option>All Territories</option>
        </select>
        <select aria-label="Product category">
          <option>All Products</option>
        </select>
      </div>
    </div>
  );
}
