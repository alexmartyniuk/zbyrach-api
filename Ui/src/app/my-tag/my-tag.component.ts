import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Tag } from '../models/tag';

@Component({
  selector: 'app-my-tag',
  templateUrl: './my-tag.component.html',
  styleUrls: ['./my-tag.component.css']
})
export class MyTagComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

  @Input() tag: Tag;

  @Output() onRemove: EventEmitter<Tag> = new EventEmitter<Tag>();

  @Output() onClick: EventEmitter<Tag> = new EventEmitter<Tag>();

  onRemoveButtonClick(): void {
    this.onRemove.emit(this.tag);
  }

  onElementClick(): void {
    this.onClick.emit(this.tag);
  }

}
