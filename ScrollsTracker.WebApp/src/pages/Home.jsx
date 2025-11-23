import { useQuery } from "@tanstack/react-query";
import MangaShelf from "../components/MangaShelf";

const baseUrl = import.meta.env.VITE_API_BASE_URL;

function Home() {
  const { data: obras = [], isLoading } = useQuery({
    queryKey: ["obras"],
    queryFn: async () => {
      const res = await fetch(`${baseUrl}/api/ScrollsTracker/ObterLancamentos`);
      return res.json();
    },
  });

  if (isLoading) {
    return (
      <div className="space-y-4 p-4 min-h-screen">
        <h1 className="text-white">Carregando...</h1>
      </div>
    );
  }

  return (
    <div className="space-y-4 p-4 min-h-screen">
      <div>
        <h1 className="text-white text-2xl">Bem-vindo ao ScrollsMaster1</h1>
      </div>
      <div className="border-b-2 border-white">
        <h1 className="text-white text-lg">Ultimos lançamentos</h1>
      </div>
      <MangaShelf obras={obras} />
    </div>
  );
}

export default Home;
