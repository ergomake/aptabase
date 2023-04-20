export function EmptyState() {
  return (
    <div className="bg-gray-50 rounded w-full h-full flex flex-col space-y-1 items-center justify-center">
      <p className="text-lg">No Data</p>
      <p className="text-sm text-secondary">There's no data available for your selection.</p>
    </div>
  );
}
