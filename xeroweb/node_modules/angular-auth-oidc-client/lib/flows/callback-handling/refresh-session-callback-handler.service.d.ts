import { Observable } from 'rxjs';
import { AuthStateService } from '../../auth-state/auth-state.service';
import { LoggerService } from '../../logging/logger.service';
import { CallbackContext } from '../callback-context';
import { FlowsDataService } from '../flows-data.service';
import * as i0 from "@angular/core";
export declare class RefreshSessionCallbackHandlerService {
    private readonly loggerService;
    private readonly authStateService;
    private readonly flowsDataService;
    constructor(loggerService: LoggerService, authStateService: AuthStateService, flowsDataService: FlowsDataService);
    refreshSessionWithRefreshTokens(configId: string): Observable<CallbackContext>;
    static ɵfac: i0.ɵɵFactoryDeclaration<RefreshSessionCallbackHandlerService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<RefreshSessionCallbackHandlerService>;
}
