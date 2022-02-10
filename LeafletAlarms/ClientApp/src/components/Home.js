"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var React = require("react");
var react_redux_1 = require("react-redux");
var MapComponent_1 = require("../map/MapComponent");
var Home = function () { return (React.createElement("div", null,
    React.createElement("h1", null, "Goodbye, world!"),
    React.createElement(MapComponent_1.MapComponent, null))); };
exports.default = (0, react_redux_1.connect)()(Home);
//# sourceMappingURL=Home.js.map