import { useQuery } from "@tanstack/react-query";
import MangaShelf from "../components/MangaShelf";

function Home() {
  const { data: obras = [], isLoading } = useQuery({
    queryKey: ["obras"],
    queryFn: async () => {
      const res = await fetch(
        "https://localhost:7071/api/ScrollsTracker/ObterLancamentos"
      );
      return res.json();
    },
  });

  if (isLoading) {
    return <h1>Carregando...</h1>;
  }

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-white">Bem-vindo ao ScrollsMaster</h1>
      </div>
      <div className="border-b-2 border-white">
        <h1 className="text-white">Ultimos lançamentos</h1>
      </div>
      <MangaShelf obras={obras} />
    </div>
  );
}

export default Home;
