import { ConfigurationProvider } from '../config/provider/config.provider';
import { LoggerService } from '../logging/logger.service';
import { StoragePersistenceService } from '../storage/storage-persistence.service';
import { RandomService } from './random/random.service';
import * as i0 from "@angular/core";
export declare class FlowsDataService {
    private storagePersistenceService;
    private randomService;
    private configurationProvider;
    private loggerService;
    constructor(storagePersistenceService: StoragePersistenceService, randomService: RandomService, configurationProvider: ConfigurationProvider, loggerService: LoggerService);
    createNonce(configId: string): string;
    setNonce(nonce: string, configId: string): void;
    getAuthStateControl(configId: string): any;
    setAuthStateControl(authStateControl: string, configId: string): void;
    getExistingOrCreateAuthStateControl(configId: string): any;
    setSessionState(sessionState: any, configId: string): void;
    resetStorageFlowData(configId: string): void;
    getCodeVerifier(configId: string): any;
    createCodeVerifier(configId: string): string;
    isSilentRenewRunning(configId: string): boolean;
    setSilentRenewRunning(configId: string): void;
    resetSilentRenewRunning(configId: string): void;
    private getSilentRenewRunningStorageEntry;
    static ɵfac: i0.ɵɵFactoryDeclaration<FlowsDataService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<FlowsDataService>;
}
