import { Component, OnInit, Input } from '@angular/core';
import { PodcastModel } from '../../models/podcasts.models';

import { Store } from '@ngrx/store';
import { AppStore } from '../../models/app.store';
import { Subscription } from 'rxjs/Subscription';
import { ActivatedRoute } from '@angular/router';

import 'rxjs/add/operator/filter';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/mergeMap';

@Component({
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html',
    styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
    @Input() podcasts: PodcastModel[];
    selectedPodcast: PodcastModel;

    constructor(private _store: Store<AppStore>, private _route: ActivatedRoute) {
        _store.select('selectedPodcast')
            .subscribe(p => {
                console.log('PodcastComponent', 'selectedPodcast', p);
                this.selectedPodcast = p;
            });
    }

    ngOnInit() {
        this._route.params.subscribe(p => {
            this._store.dispatch({ type: 'FIND_ITEM', payload: p });
        });
    }

    deletePodcast(podcast) {

    }

    selectPodcast(podcast) {
        console.log('SidebarComponent', 'selectePodcast', podcast);
        this._store.dispatch({ type: 'SELECT_ITEM', payload: podcast });
    }
}