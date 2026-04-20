import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { LiftsPageFacade } from './lifts-page.facade';

@Component({
  selector: 'app-lifts-page',
  imports: [CommonModule, FormsModule],
  templateUrl: './lifts-page.component.html',
  styleUrl: './lifts-page.component.scss',
})
export class LiftsPageComponent implements OnInit {
  readonly facade = inject(LiftsPageFacade);

  ngOnInit(): void {
    this.facade.load();
  }
}
