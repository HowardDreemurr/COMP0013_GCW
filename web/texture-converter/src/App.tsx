import "./App.css";
import MinecraftSkinMapper from "./section/converter";
import { HeroUIProvider } from "@heroui/react";

function App() {
  return (
    <HeroUIProvider>
      <div className="App">
        <MinecraftSkinMapper />
      </div>
    </HeroUIProvider>
  );
}

export default App;
