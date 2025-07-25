import React from "react";
import { Link } from "react-router-dom";

// 1. Importe a sua imagem como se fosse um módulo JavaScript.
// O caminho relativo deve ser a partir DESTE arquivo (Navbar.jsx) até a sua imagem.
// Ajuste o caminho '../assets/logo.jpg' se necessário.
import logo from "../assets/logo.png";

function Navbar() {
  return (
    <header className="bg-orange-400 text-black p-4 border-b-4 border-indigo-500 shadow-md">
      {/* Container para alinhar os itens com margem automática */}
      <div className="container mx-auto flex justify-between items-center">
        {/* Logo (à esquerda) */}
        <Link to="/" className="flex items-center gap-3">
          {/* 2. Use a variável importada no atributo 'src' da tag <img> */}
          <img
            src={logo}
            alt="ScrollsTracker Logo"
            className="h-10 w-10 object-cover" // Ajustei o estilo para a imagem
          />
          <span className="text-xl font-bold text-gray-800">
            ScrollsTracker
          </span>
        </Link>

        {/* Navegação (à direita) */}
        <nav>
          <ul className="flex items-center gap-6">
            <li>
              <Link
                to="/"
                className="text-lg font-medium hover:text-indigo-700 transition-colors"
              >
                Home
              </Link>
            </li>
            <li>
              <Link
                to="/biblioteca"
                className="text-lg font-medium hover:text-indigo-700 transition-colors"
              >
                Biblioteca
              </Link>
            </li>
          </ul>
        </nav>
      </div>
    </header>
  );
}

export default Navbar;
