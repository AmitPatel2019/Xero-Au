import { Observable } from 'rxjs';
import { LoggerService } from '../../logging/logger.service';
import { CallbackContext } from '../callback-context';
import { FlowsDataService } from '../flows-data.service';
import { ResetAuthDataService } from '../reset-auth-data.service';
import * as i0 from "@angular/core";
export declare class ImplicitFlowCallbackHandlerService {
    private readonly resetAuthDataService;
    private readonly loggerService;
    private readonly flowsDataService;
    private readonly doc;
    constructor(resetAuthDataService: ResetAuthDataService, loggerService: LoggerService, flowsDataService: FlowsDataService, doc: any);
    implicitFlowCallback(configId: string, hash?: string): Observable<CallbackContext>;
    static ɵfac: i0.ɵɵFactoryDeclaration<ImplicitFlowCallbackHandlerService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<ImplicitFlowCallbackHandlerService>;
}
