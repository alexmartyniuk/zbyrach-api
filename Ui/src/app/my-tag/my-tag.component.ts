import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-my-tag',
  templateUrl: './my-tag.component.html',
  styleUrls: ['./my-tag.component.css']
})
export class MyTagComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

  @Input() value: string;

  @Output() onRemove: EventEmitter<String> = new EventEmitter<String>();

  @Output() onClick: EventEmitter<String> = new EventEmitter<String>();

  onRemoveButtonClick(): void {
    this.onRemove.emit(this.value);
  }

  onElementClick(): void {
    this.onClick.emit(this.value);
  }

}
