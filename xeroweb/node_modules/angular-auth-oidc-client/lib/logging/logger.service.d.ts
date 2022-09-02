import { ConfigurationProvider } from '../config/provider/config.provider';
import * as i0 from "@angular/core";
export declare class LoggerService {
    private configurationProvider;
    constructor(configurationProvider: ConfigurationProvider);
    logError(configId: string, message: any, ...args: any[]): void;
    logWarning(configId: string, message: any, ...args: any[]): void;
    logDebug(configId: string, message: any, ...args: any[]): void;
    private logWarningWithoutConfig;
    private logWarningWithConfig;
    private logErrorWithConfig;
    private logErrorWithoutConfig;
    private currentLogLevelIsEqualOrSmallerThan;
    private logLevelIsSet;
    private loggingIsTurnedOff;
    static ɵfac: i0.ɵɵFactoryDeclaration<LoggerService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<LoggerService>;
}
