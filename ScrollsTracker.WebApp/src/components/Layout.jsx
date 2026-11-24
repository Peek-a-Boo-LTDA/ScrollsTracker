import { Navigate, Outlet } from "react-router-dom";
import { useSessao } from "../useSessao";
import Navbar from "./Navbar";

function Layout() {
  const sessao = useSessao();

  if (sessao.isPending) return <p className="p-4 text-white">Carregando sessão...</p>;
  if (sessao.isError) {
    return (
      <button onClick={() => sessao.refetch()} className="p-4 text-white">
        Falha ao verificar a sessão. Tentar novamente
      </button>
    );
  }
  if (!sessao.data) return <Navigate to="/login" replace />;

  return (
    <div>
      <Navbar />
      <main className="app-main">
        <Outlet context={sessao.data} />
      </main>
    </div>
  );
}

export default Layout;
