import { Observable } from 'rxjs';
import { AuthOptions } from '../auth-options';
import { ConfigurationProvider } from '../config/provider/config.provider';
import { StoragePersistenceService } from '../storage/storage-persistence.service';
import { LoginResponse } from './login-response';
import { ParLoginService } from './par/par-login.service';
import { PopUpLoginService } from './popup/popup-login.service';
import { PopupOptions } from './popup/popup-options';
import { StandardLoginService } from './standard/standard-login.service';
import * as i0 from "@angular/core";
export declare class LoginService {
    private configurationProvider;
    private parLoginService;
    private popUpLoginService;
    private standardLoginService;
    private storagePersistenceService;
    constructor(configurationProvider: ConfigurationProvider, parLoginService: ParLoginService, popUpLoginService: PopUpLoginService, standardLoginService: StandardLoginService, storagePersistenceService: StoragePersistenceService);
    login(configId: string, authOptions?: AuthOptions): void;
    loginWithPopUp(configId: string, authOptions?: AuthOptions, popupOptions?: PopupOptions): Observable<LoginResponse>;
    static ɵfac: i0.ɵɵFactoryDeclaration<LoginService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<LoginService>;
}
