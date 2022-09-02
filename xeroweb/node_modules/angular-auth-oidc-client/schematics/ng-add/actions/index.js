"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.getAllActions = void 0;
const add_dependencies_1 = require("./add-dependencies");
const add_module_import_1 = require("./add-module-import");
const adding_entry_to_assets_1 = require("./adding-entry-to-assets");
const copy_module_file_1 = require("./copy-module-file");
const copy_silent_renew_html_1 = require("./copy-silent-renew-html");
const install_dependencies_1 = require("./install-dependencies");
const schema_parser_1 = require("./schema-parser");
function getAllActions(options) {
    const ngAddOptions = (0, schema_parser_1.parseSchema)(options);
    return [
        (0, add_dependencies_1.addPackageJsonDependencies)(ngAddOptions),
        (0, install_dependencies_1.installPackageJsonDependencies)(),
        (0, copy_module_file_1.copyModuleFile)(ngAddOptions),
        (0, add_module_import_1.addModuleToImports)(ngAddOptions),
        (0, adding_entry_to_assets_1.addSilentRenewHtmlToAssetsArrayInAngularJson)(ngAddOptions),
        (0, copy_silent_renew_html_1.copySilentRenewHtmlToRoot)(ngAddOptions),
    ];
}
exports.getAllActions = getAllActions;
//# sourceMappingURL=index.js.map