import React, {useContext, useReducer} from 'react'

const EventContext = React.createContext()

export const useEvent = () => {
  return useContext(EventContext)
}

const SHOW_ALERT = 'show'

const reducer = (state, action) => {
  switch (action.type) {
    case SHOW_ALERT: return {...state, counter: state.counter + 1, text: action.text}
    default: return state
  }
}

export const EventProvider = ({ children }) => {
  const [state, dispatch] = useReducer(reducer, {
    counter: 1,
    text: ''
  })

  const show = text => dispatch({ type: SHOW_ALERT, text })

  return (
    <EventContext.Provider value={{
      counter: state.counter,
      text: state.text,
      show
    }}>
        { children }
    </EventContext.Provider>
  )
}