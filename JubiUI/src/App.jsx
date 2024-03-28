import './App.css'
import BotControls from './components/BotControls'


function App() {
    return (
        <>
            <BotControls />

            <a
                className="kofiBtn"
                href="https://ko-fi.com/R6R4NI4T0"
                target="_blank"
                rel="noreferrer"
            >
                <img
                    height="36"
                    src="https://storage.ko-fi.com/cdn/kofi1.png?v=3"
                    border="0"
                    alt="Buy Me a Coffee at ko-fi.com"
                />
            </a>
        </>
    )
}

export default App
