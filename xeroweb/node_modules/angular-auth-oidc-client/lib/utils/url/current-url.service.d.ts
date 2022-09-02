import * as i0 from "@angular/core";
export declare class CurrentUrlService {
    private doc;
    constructor(doc: any);
    getStateParamFromCurrentUrl(): string;
    currentUrlHasStateParam(): boolean;
    getCurrentUrl(): string;
    static ɵfac: i0.ɵɵFactoryDeclaration<CurrentUrlService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<CurrentUrlService>;
}
