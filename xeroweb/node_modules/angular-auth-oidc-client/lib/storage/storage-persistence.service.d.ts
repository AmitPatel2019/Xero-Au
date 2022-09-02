import { BrowserStorageService } from './browser-storage.service';
import * as i0 from "@angular/core";
export declare type StorageKeys = 'authnResult' | 'authzData' | 'access_token_expires_at' | 'authWellKnownEndPoints' | 'userData' | 'authNonce' | 'codeVerifier' | 'authStateControl' | 'session_state' | 'storageSilentRenewRunning' | 'storageCustomParamsAuthRequest' | 'storageCustomParamsRefresh' | 'storageCustomParamsEndSession' | 'redirect' | 'configIds' | 'jwtKeys';
export declare class StoragePersistenceService {
    private readonly browserStorageService;
    constructor(browserStorageService: BrowserStorageService);
    read(key: StorageKeys, configId: string): any;
    write(key: StorageKeys, value: any, configId: string): void;
    remove(key: StorageKeys, configId: string): void;
    clear(configId: string): void;
    resetStorageFlowData(configId: string): void;
    resetAuthStateInStorage(configId: string): void;
    getAccessToken(configId: string): string;
    getIdToken(configId: string): string;
    getRefreshToken(configId: string): string;
    getAuthenticationResult(configId: string): any;
    static ɵfac: i0.ɵɵFactoryDeclaration<StoragePersistenceService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<StoragePersistenceService>;
}
