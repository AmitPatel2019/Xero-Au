import { ConfigurationProvider } from '../config/provider/config.provider';
import { LoggerService } from '../logging/logger.service';
import * as i0 from "@angular/core";
export declare class BrowserStorageService {
    private configProvider;
    private loggerService;
    constructor(configProvider: ConfigurationProvider, loggerService: LoggerService);
    read(key: string, configId: string): any;
    write(value: any, configId: string): boolean;
    remove(key: string, configId: string): boolean;
    clear(configId: string): boolean;
    private getStorage;
    private hasStorage;
    static ɵfac: i0.ɵɵFactoryDeclaration<BrowserStorageService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<BrowserStorageService>;
}
