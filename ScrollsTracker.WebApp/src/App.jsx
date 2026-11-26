import { createBrowserRouter, RouterProvider } from "react-router-dom";
import Layout from "./components/Layout";
import Biblioteca from "./pages/Biblioteca";
import Home from "./pages/Home";
import CadastrarObra from "./pages/CadastrarObra";
import AtualizarObra from "./pages/AtualizarObra";
import Conta from "./pages/Conta";
import ImportarObra from "./pages/ImportarObras";
import DiagnosticoFontes from "./pages/DiagnosticoFontes";

const router = createBrowserRouter([
  { path: "/login", element: <Conta key="login" /> },
  { path: "/cadastro", element: <Conta cadastro key="cadastro" /> },
  {
    element: <Layout />,
    children: [
      {
        path: "/",
        element: <Home />,
      },
      {
        path: "/biblioteca",
        element: <Biblioteca />,
      },
      {
        path: "/cadastrar-obra",
        element: <CadastrarObra />,
      },
      {
        path: "/atualizar-obra/:id",
        element: <AtualizarObra />,
      },
      {
        path: "/importar",
        element: <ImportarObra />,
      },
      { path: "/diagnostico-fontes", element: <DiagnosticoFontes /> },
    ],
  },
]);

// O componente App agora é responsável por prover o roteamento para a aplicação
function App() {
  return <RouterProvider router={router} />;
}

export default App;
