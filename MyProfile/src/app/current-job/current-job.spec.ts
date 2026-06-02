import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CurrentJob } from './current-job';

describe('CurrentJob', () => {
  let component: CurrentJob;
  let fixture: ComponentFixture<CurrentJob>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CurrentJob],
    }).compileComponents();

    fixture = TestBed.createComponent(CurrentJob);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
