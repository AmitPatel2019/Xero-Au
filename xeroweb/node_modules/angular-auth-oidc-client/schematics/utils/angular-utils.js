"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.readIntoSourceFile = exports.getProject = exports.updateProjectInAngularJson = exports.getAngularWorkspace = exports.ANGULAR_JSON_FILENAME = void 0;
const schematics_1 = require("@angular-devkit/schematics");
const ts = require("@schematics/angular/third_party/github.com/Microsoft/TypeScript/lib/typescript");
exports.ANGULAR_JSON_FILENAME = 'angular.json';
function getAngularWorkspace(tree) {
    const workspaceContent = getAngularJsonContent(tree);
    const workspace = JSON.parse(workspaceContent);
    return workspace;
}
exports.getAngularWorkspace = getAngularWorkspace;
function updateProjectInAngularJson(tree, content, projectName) {
    projectName = projectName || getDefaultProjectName(tree);
    if (!projectName) {
        throw new schematics_1.SchematicsException(`project '${projectName}' could not be found and no default project is given in workspace`);
    }
    const workspaceContent = getAngularJsonContent(tree);
    const workspace = JSON.parse(workspaceContent);
    workspace['projects'][projectName] = content;
    tree.overwrite(exports.ANGULAR_JSON_FILENAME, JSON.stringify(workspace, null, 2));
}
exports.updateProjectInAngularJson = updateProjectInAngularJson;
function getProject(tree, projectName) {
    const workspace = getAngularWorkspace(tree);
    const hasProjectName = !!projectName;
    const hasDefaultProject = !!workspace.defaultProject;
    if (hasProjectName) {
        return workspace.projects[projectName] || null;
    }
    else if (hasDefaultProject) {
        return workspace.projects[workspace.defaultProject];
    }
    throw new schematics_1.SchematicsException(`project '${projectName}' could not be found and no default project is given in workspace`);
}
exports.getProject = getProject;
function readIntoSourceFile(host, fileName) {
    const buffer = host.read(fileName);
    if (buffer === null) {
        throw new schematics_1.SchematicsException(`File ${fileName} does not exist.`);
    }
    return ts.createSourceFile(fileName, buffer.toString('utf-8'), ts.ScriptTarget.Latest, true);
}
exports.readIntoSourceFile = readIntoSourceFile;
function getAngularJsonContent(tree) {
    const workspaceConfig = tree.read(exports.ANGULAR_JSON_FILENAME);
    if (!workspaceConfig) {
        throw new schematics_1.SchematicsException('Could not find Angular workspace configuration');
    }
    return workspaceConfig.toString();
}
function getDefaultProjectName(tree) {
    const workspace = getAngularWorkspace(tree);
    return workspace.defaultProject;
}
//# sourceMappingURL=angular-utils.js.map