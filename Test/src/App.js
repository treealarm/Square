import React, { useState, useMemo, useCallback } from "react";

import { GOT_TIMER_RESTART } from "./Store.ts";

import { Provider } from "react-redux";
import { configureStore } from "./Store.ts";

import logo from "./logo.svg";
import "./App.css";
import { Page } from "./Page.tsx";
import { TimerVal } from "./Timer.tsx";
import AlarmButton from "./AlarmButton.js";
import { EventProvider, useEvent } from "./Events/EventContext.js";

const mainStore = configureStore();

function App() {

  const [count, setCount] = useState(1);

  function onClickStart() {
    console.log("onClickStart");
    mainStore.dispatch({ type: GOT_TIMER_RESTART });
  }

  function onClickAddCircle() {
    console.log("onClickAddCircle");

  }

  function onTimerTickCame(timerVal1) {
    setCount(timerVal1);
  }

  const generateItemsFromAPI = useCallback(() => {
    return count;
  }, [count])
  
  return (
    <Provider store={mainStore}>
      <EventProvider>
        <div className="App">
          <Page timerVal={generateItemsFromAPI()} store={mainStore}></Page>      
          <TimerVal store={mainStore} onTimerTick={onTimerTickCame} />
          <button onClick={onClickStart}>Go to start pos</button>;
          <AlarmButton/>
        </div>
      </EventProvider>
    </Provider>
  );
}

export default App;
