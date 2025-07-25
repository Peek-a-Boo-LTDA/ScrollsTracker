import { Outlet } from "react-router-dom";
import Navbar from "./Navbar";

function Layout() {
  return (
    <div>
      <Navbar />
      <main className="bg-stone-900">
        {/* O conteúdo da página específica da rota será renderizado aqui */}
        <Outlet />
      </main>
    </div>
  );
}

export default Layout;
