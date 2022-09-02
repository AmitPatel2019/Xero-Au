import { AuthStateService } from '../auth-state/auth-state.service';
import { UserService } from '../user-data/user.service';
import { FlowsDataService } from './flows-data.service';
import * as i0 from "@angular/core";
export declare class ResetAuthDataService {
    private readonly authStateService;
    private readonly flowsDataService;
    private readonly userService;
    constructor(authStateService: AuthStateService, flowsDataService: FlowsDataService, userService: UserService);
    resetAuthorizationData(configId: string): void;
    static ɵfac: i0.ɵɵFactoryDeclaration<ResetAuthDataService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<ResetAuthDataService>;
}
