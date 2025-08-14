import ReactDOM from "react-dom";

// O componente Modal genérico e reutilizável
function Modal({ isOpen, onClose, children }) {
  // Se não estiver aberto, não renderiza nada
  if (!isOpen) return null;

  // Usamos createPortal para renderizar o modal fora da hierarquia do componente pai,
  // diretamente no elemento 'portal' do body.
  return ReactDOM.createPortal(
    <>
      {/* Overlay: Fundo escuro que cobre a tela */}
      <div
        className="fixed inset-0 bg-black opacity-75 z-40"
        onClick={onClose} // Fecha o modal ao clicar no fundo
      ></div>

      {/* Conteúdo do Modal: A caixa central */}
      <div className="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 bg-white p-8 rounded-lg shadow-2xl z-50 w-full max-w-lg">
        {/* Botão de fechar no canto superior direito */}
        <button
          onClick={onClose}
          className="absolute top-3 right-3 text-gray-500 hover:text-gray-800 text-2xl font-bold"
        >
          &times; {/* Entidade HTML para um "X" elegante */}
        </button>

        {/* O conteúdo que for passado para o modal será renderizado aqui */}
        {children}
      </div>
    </>,
    document.getElementById("portal")
  );
}

export default Modal;
