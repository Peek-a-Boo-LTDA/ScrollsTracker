import { useQuery } from "@tanstack/react-query";
import MangaShelf from "../components/MangaShelf";
import { Link } from "react-router-dom";

const baseUrl = import.meta.env.VITE_API_BASE_URL;

function Biblioteca() {
  const { data: obras = [], isLoading } = useQuery({
    queryKey: ["obras"],
    queryFn: async () => {
      const res = await fetch(`${baseUrl}/api/ScrollsTracker/Obras`);
      return res.json();
    },
  });

  if (isLoading) {
    return <h1>Carregando...</h1>;
  }

  return (
    <div className="space-y-4 p-4">
      <div className="flex justify-between items-center pl-2 pr-2 mb-1 pt-4">
        <h1 className="text-white text-2xl">Biblioteca</h1>
        <Link
          to="/cadastrar-obra"
          className="text-white lg:block bg-transparent text-primary border hover:bg-primary border-primary hover:text-darkmode px-4 py-2 rounded-lg cursor-pointer"
        >
          Nova Obra
        </Link>
      </div>
      <div className="border-b-2 border-white pl-2 pr-2">
        <h1 className="text-white text-lg">Obras:</h1>
      </div>
      <MangaShelf obras={obras} />
    </div>
  );
}

export default Biblioteca;
