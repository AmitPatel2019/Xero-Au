import { LoggerService } from '../logging/logger.service';
import * as i0 from "@angular/core";
export declare class IFrameService {
    private readonly doc;
    private loggerService;
    constructor(doc: any, loggerService: LoggerService);
    getExistingIFrame(identifier: string): HTMLIFrameElement | null;
    addIFrameToWindowBody(identifier: string, configId: string): HTMLIFrameElement;
    private getIFrameFromParentWindow;
    private getIFrameFromWindow;
    private isIFrameElement;
    static ɵfac: i0.ɵɵFactoryDeclaration<IFrameService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<IFrameService>;
}
