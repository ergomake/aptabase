import { ExclamationTriangleIcon } from "@heroicons/react/24/outline";

export function ErrorState() {
  return (
    <div className="w-full h-full text-red-700 flex flex-col space-y-1 items-center justify-center">
      <p className="text-lg flex items-center space-x-2">
        <ExclamationTriangleIcon className="h-5 w-5" />
        <span>Oops... Something went wrong.</span>
      </p>
      <p className="text-sm text-secondary">Please try again. If the problem persists, please contact support.</p>
    </div>
  );
}
