import { AbstractSecurityStorage } from './abstract-security-storage';
import * as i0 from "@angular/core";
export declare class DefaultSessionStorageService implements AbstractSecurityStorage {
    read(key: string): any;
    write(key: string, value: any): void;
    remove(key: string): void;
    clear(): void;
    static ɵfac: i0.ɵɵFactoryDeclaration<DefaultSessionStorageService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<DefaultSessionStorageService>;
}
