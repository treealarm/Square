import React from 'react'
import {useEvent} from './Events/EventContext'

export default function AlarmButton() {
  const {show} = useEvent()
  return (
    <>
      <button onClick={() => show('Этот текст из Main.js')} className="btn btn-success">Add circle</button>
    </>
  )
}