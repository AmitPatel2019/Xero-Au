import { Observable } from 'rxjs';
import { PopupOptions } from './popup-options';
import { PopupResult } from './popup-result';
import * as i0 from "@angular/core";
export declare class PopUpService {
    private STORAGE_IDENTIFIER;
    private popUp;
    private handle;
    private resultInternal$;
    get result$(): Observable<PopupResult>;
    isCurrentlyInPopup(): boolean;
    openPopUp(url: string, popupOptions?: PopupOptions): void;
    sendMessageToMainWindow(url: string): void;
    private cleanUp;
    private sendMessage;
    private getOptions;
    private canAccessSessionStorage;
    static ɵfac: i0.ɵɵFactoryDeclaration<PopUpService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<PopUpService>;
}
