"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ngAdd = void 0;
const schematics_1 = require("@angular-devkit/schematics");
const actions_1 = require("./actions");
function ngAdd(options) {
    const allActions = (0, actions_1.getAllActions)(options);
    return (0, schematics_1.chain)(allActions);
}
exports.ngAdd = ngAdd;
//# sourceMappingURL=index.js.map