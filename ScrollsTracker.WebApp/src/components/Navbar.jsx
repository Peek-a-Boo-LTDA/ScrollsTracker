import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiFetch } from "../api";
import { chaveSessao } from "../useSessao";
import { Link, NavLink } from "react-router-dom";

import logo from "../assets/logo.png";

function Navbar() {
  const queryClient = useQueryClient();
  const logout = useMutation({
    mutationFn: async () => {
      const resposta = await apiFetch("/api/conta/logout", { method: "POST" });
      if (!resposta.ok && resposta.status !== 401) throw new Error("Falha ao sair.");
    },
    onSuccess: async () => {
      await queryClient.cancelQueries();
      queryClient.setQueryData(chaveSessao, null);
      queryClient.removeQueries({ predicate: (query) => query.queryKey[0] !== chaveSessao[0] });
    },
  });
  return (
    <header className="app-header">
      <div className="page-width flex flex-wrap items-center justify-between gap-x-6 gap-y-3 py-3">
        <Link to="/" className="flex items-center gap-3 font-semibold tracking-tight text-slate-100">
          <img src={logo} alt="" className="size-9 rounded-lg bg-slate-200 object-contain p-0.5" />
          <span>ScrollsTracker</span>
        </Link>
        <nav aria-label="Navegação principal">
          <ul className="flex flex-wrap items-center gap-1 sm:gap-2">
            <li><NavLink end to="/" className="nav-item">Lançamentos</NavLink></li>
            <li><NavLink to="/biblioteca" className="nav-item">Biblioteca</NavLink></li>
            <li><NavLink to="/importar" className="nav-item">Importar</NavLink></li>
            <li>
              <button type="button" onClick={() => logout.mutate()} disabled={logout.isPending} className="nav-item nav-exit">
                Sair
              </button>
            </li>
          </ul>
        </nav>
        {logout.isError && <p role="alert" className="w-full text-sm text-red-300">Falha ao sair. Tente novamente.</p>}
      </div>
    </header>
  );
}

export default Navbar;
