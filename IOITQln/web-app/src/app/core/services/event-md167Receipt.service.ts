import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class EventMd167ReceiptService {

	private subject = new Subject<any>();

	sendMessage() {
		this.subject.next(true);
	}

	clearMessage() {
		this.subject.next(undefined);
	}

	getMessage(): Observable<any> {
		return this.subject.asObservable();
	}
}
