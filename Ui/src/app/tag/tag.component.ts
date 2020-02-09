import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Tag } from '../models/tag';

@Component({
  selector: 'mg-tag',
  templateUrl: './tag.component.html',
  styleUrls: ['./tag.component.css']
})
export class TagComponent implements OnInit {

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
